'use strict';

var clientFromConnectionString = require('azure-iot-device-mqtt').clientFromConnectionString;
var Message = require('azure-iot-device').Message;
var client = clientFromConnectionString(require('./settings.js').settings.deviceConnection);

function printResultFor(op) {
  return function printResult(err, res) {
    if (err) console.log(op + ' error: ' + err.toString());
    if (res) console.log(op + ' status: ' + res.constructor.name);
  };
}

 var connectCallback = function (err) {
   if (err) {
     console.log('Could not connect: ' + err);
   } else {
     console.log('Client connected');
     client.on('message', function (msg) {
       console.log('Id: ' + msg.messageId + ' Body: ' + msg.data);
       client.complete(msg, printResultFor('completed'));
     });
   }
 };

client.open(connectCallback);