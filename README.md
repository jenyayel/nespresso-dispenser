#Nespresso Dispenser server parts

[Azure Functions](https://azure.microsoft.com/en-us/services/functions/) that serves as a WebHook for [Slash Commands](https://api.slack.com/slash-commands) and publishes 
cloud-to-device messages.

There are currently two separate implementations of the same thing - first publishes to [Azure IoT Hub](https://azure.microsoft.com/en-us/services/iot-hub/) and the second to generic MQTT topic.

### Misc

The folder `node-iot-demo` implements 3 operations for IoT Hub - register device,
receive cloud-to-device messages, publish cloud-to-device messages using [Azure IoT SDK](https://github.com/azure/azure-iot-sdk-node/).
