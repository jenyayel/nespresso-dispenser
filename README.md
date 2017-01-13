#Nespresso Dispenser server parts

Azure function that serves as a WebHook for [Slash Commands](https://api.slack.com/slash-commands) and publishes 
cloud-to-device messages. The connection string `iot-hub` must be defined to [IoT Hub](https://azure.microsoft.com/en-us/services/iot-hub/).

### Misc

The folder `node-iot-demo` implements 3 operations for IoT Hub - register device,
receive cloud-to-device messages, publish cloud-to-device messages using [Azure IoT SDK](https://github.com/azure/azure-iot-sdk-node/).
