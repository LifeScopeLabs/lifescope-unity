'use strict';

const assert = require('assert');

const Sequelize = require('sequelize');
const _ = require('lodash');
const cookie = require('cookie');
const nunjucks = require('nunjucks');
const request = require('request-promise');


let renderer = nunjucks.configure('templates');


module.exports = async function(event, context) {
	let html, sequelize;

	let cookies = _.get(event, 'headers.cookie', '');
	let sessionId = cookie.parse(cookies).unity_session_id;

	let query = event.queryStringParameters || {};
	let code = query.code;

	if (!sessionId && !code || code == null) {
		callback(null, {
			statusCode: 404,
			body: JSON.stringify({
				sessionId: sessionId,
				event: event,
				cookies: cookies
			})
		});
	}
	else {
		try {
			assert(process.env.HOST != null, 'Unspecified RDS host.');
			assert(process.env.PORT != null, 'Unspecified RDS port.');
			assert(process.env.USER != null, 'Unspecified RDS user.');
			assert(process.env.PASSWORD != null, 'Unspecified RDS password.');
			assert(process.env.DATABASE != null, 'Unspecified RDS database.');

			sequelize = new Sequelize(process.env.DATABASE, process.env.USER, process.env.PASSWORD, {
				host: process.env.HOST,
				port: process.env.PORT,
				dialect: 'mysql'
			});

			let authSessions = sequelize.define('auth_session', {
				id: {
					type: Sequelize.INTEGER,
					primaryKey: true,
					autoIncrement: true
				},
				access_code: {
					type: Sequelize.STRING
				},
				created: {
					type: Sequelize.DATE,
					defaultValue: Sequelize.NOW
				},
				oauth_token: {
					type: Sequelize.STRING
				},
				token: {
					type: Sequelize.STRING
				}
			}, {
				timestamps: false
			});

			await authSessions.sync();

			let session = await authSessions.find({
				where: {
					token: sessionId
				}
			});

			if (session == null) {
				html = renderer.render('error.html', {});

				return {
					statusCode: 200,
					headers: {
						'Content-Type': 'text/html',
						'Access-Control-Allow-Origin': '*'
					},
					body: html
				};
			}
			else {
				let body = {
					client_id: process.env.CLIENT_ID,
					client_secret: process.env.CLIENT_SECRET,
					grant_type: 'authorization_code',
					code: code,
					redirect_uri: process.env.SITE_URL + '/complete'
				};

				let accessTokenResponse = await request({
					method: 'POST',
					url: 'https://api.lifescope.io/auth/access_token',
					body: body,
					json: true
				});

				if (accessTokenResponse.code && accessTokenResponse.code > 399) {
					throw new Error(accessTokenResponse.message);
				}

				let accessCode = await getUniqueAccessCode(authSessions);

				await authSessions.update({
					access_code: accessCode,
					oauth_token: accessTokenResponse.access_token
				}, {
					where: {
						token: sessionId
					}
				});

				html = renderer.render('access_code.html', {
					access_code: accessCode
				});

				await sequelize.close();

				return {
					statusCode: 200,
					headers: {
						'Content-Type': 'text/html',
						'Access-Control-Allow-Origin': '*'
					},
					body: html
				};
			}
		} catch(err) {
			console.log('COMPLETE ERROR');
			console.log(err);

			if (sequelize) {
				await sequelize.close()
			}

			throw err;
		}

	}
};

function randomString(length, chars) {
	let result = '';
	
	for (let i = length; i > 0; --i) result += chars[Math.floor(Math.random() * chars.length)];
	
	return result;
}

async function getUniqueAccessCode(authSessions) {
	let accessCode = randomString(8, '1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ');

	let result = await authSessions.findOne({
		where: {
			access_code: accessCode
		}
	});

	if (result != null) {
		return getUniqueAccessCode(authSessions);
	}
	else {
		return accessCode;
	}
}