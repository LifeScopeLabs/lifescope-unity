'use strict';

const complete = require('./views/complete.js');
const createSession =require('./views/create_session');
const exchangeCode = require('./views/exchange_code');


exports.handler = async function(event, context) {
	let path = event.path;

	if (path === '/complete') {
		return complete(event, context);
	}
	else if (path === '/create_session' || path === '/create-session') {
		return createSession(event, context);
	}
	else if (path === '/exchange_code' || path === '/exchange-code') {
		return exchangeCode(event, context);
	}
	else {
		return {
			statusCode: 400,
			body: 'Invalid path'
		};
	}
};
