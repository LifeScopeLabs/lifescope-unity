'use strict';

const assert = require('assert');

const Sequelize = require('sequelize');
const _ = require('lodash');
const cookie = require('cookie');


module.exports = async function(event, context) {
	let html, sequelize;

	let cookies = _.get(event, 'headers.cookie', '');
	let sessionId = cookie.parse(cookies).unity_session_id;

	let query = event.queryStringParameters || {};
	let accessCode = query.access_code;

	if (!sessionId && !accessCode || accessCode == null) {
		return {
			statusCode: 404,
			body: JSON.stringify({
				sessionId: sessionId,
				event: event,
				cookies: cookies
			})
		};
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
					token: sessionId,
					access_code: accessCode
				}
			});

			if (session == null) {
				await sequelize.close();

				return {
					statusCode: 400,
					body: 'session did not exist'
				};
			}
			else {
				await authSessions.destroy({
					where: {
						token: sessionId
					}
				});

				await sequelize.close();

				return {
					statusCode: 200,
					body: JSON.stringify({
						oauth_token: session.oauth_token
					})
				};
			}
		} catch(err) {
			console.log('EXCHANGE TOKEN ERROR');
			console.log(err);

			if (sequelize) {
				await sequelize.close()
			}

			throw err;
		}

	}
};

//https://r4o1ekqi1d.execute-api.us-east-1.amazonaws.com/dev/create-session?client_id=e33af1dc0124dbf5&scope=events:read