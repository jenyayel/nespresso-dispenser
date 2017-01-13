'use strict';

var Client = require('azure-iothub').Client;
var Message = require('azure-iot-common').Message;
var settings = require('./settings.js').settings;

var serviceClient = Client.fromConnectionString(settings.cloudConnection);

function printResultFor(op) {
    return function printResult(err, res) {
        if (err) console.log(op + ' error: ' + err.toString());
        if (res) console.log(op + ' status: ' + res.constructor.name);
    };
}

function receiveFeedback(err, receiver) {
    receiver.on('message', function (msg) {
        console.log('Feedback message:')
        console.log(msg.getData().toString('utf-8'));
    });
}

serviceClient.open(function (err) {
    if (err) {
        console.error('Could not connect: ' + err.message);
    } else {
        console.log('Service client connected');
        serviceClient.getFeedbackReceiver(receiveFeedback);

        setInterval(function () {
            var message = new Message('Cloud to device message ' + (Math.random() * 4));
            message.ack = 'full';
            message.messageId = (new Date().getTime() * 10000) + '';
            console.log('Sending message: ' + message.getData());
            serviceClient.send(settings.deviceId, message, printResultFor('send'));

        }, 1000);
    }
});