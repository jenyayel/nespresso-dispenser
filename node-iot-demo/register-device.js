'use strict';

var iothub = require('azure-iothub');
var settings = require('./settings.js').settings;
var registry = iothub.Registry.fromConnectionString(settings.cloudConnection);

var device = new iothub.Device(null);
device.deviceId = settings.deviceId;
registry.create(device, function (err, deviceInfo, res) {
    if (err) {
        registry.get(device.deviceId, printDeviceInfo);
    }
    if (deviceInfo) {
        printDeviceInfo(err, deviceInfo, res)
    }
});

function printDeviceInfo(err, deviceInfo, res) {
    console.log(err);
    if (deviceInfo) {
        console.log('Device ID: ' + deviceInfo.deviceId);
        console.log('Device key: ' + deviceInfo.authentication.symmetricKey.primaryKey);
    }
}