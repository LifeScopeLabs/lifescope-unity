'use strict';

const assert = require('assert');

const Sequelize = require('sequelize');


module.exports = async function(event, context) {
	let sequelize;

	let query = event.queryStringParameters || {};
	let accessCode = query.access_code;

	if (!accessCode || accessCode == null) {
		return {
			statusCode: 404,
			body: JSON.stringify({
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

			let session = await authSessions.findOne({
				where: {
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
						access_code: accessCode
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
