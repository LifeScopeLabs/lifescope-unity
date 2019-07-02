'use strict';

const assert = require('assert');
const url = require('url');

const Sequelize = require('sequelize');
const moment = require('moment');
const uuid = require('uuid');


module.exports = async function(event, context) {
	let sequelize;
	let scopes = event.queryStringParameters.scopes;
	let state = '12345678';
	
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

		let token = uuid().replace(/-/g, '');
		let expiration = moment.utc().add(300, 'seconds').toDate();

		let authSessions = await sequelize.define('auth_session', {
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

		let domain = url.parse(process.env.SITE_URL).hostname;

		let cookieString = 'unity_session_id=' + token + '; domain=' + domain + '; expires=' + expiration + '; secure=true; http_only=true';
		let redirectUrl = 'https://app.lifescope.io/auth?client_id=' + process.env.CLIENT_ID + '&redirect_uri=' + process.env.SITE_URL + '/complete&response_type=code&state=' + state + '&scope=' + scopes;

		await authSessions.sync();

		await authSessions.destroy({
			where: {
				created: {
					$lte: moment().subtract(5, 'minutes').toDate()
				}
			}
		});

		await authSessions.create({
			token: token
		});

		await sequelize.close();

		return {
			statusCode: 302,
			headers: {
				'Set-Cookie' : cookieString,
				Location: redirectUrl
			}
		};
	} catch(err) {
		console.log('CREATE_SESSION ERROR');
		console.log(err);

		if (sequelize) {
			await sequelize.close();
		}

		return {
			statusCode: 404,
			body: err.toString()
		};
	}
};