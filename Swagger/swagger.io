swagger: '2.0'
info:
  title: ASCOM Web
  description: Provides access to ASCOM devices remotely over the web
  version: "1.0.0"
# the domain of the service
host: localhost:11111
# array of all schemes that your API supports
schemes:
  - http
  - https
# will be prefixed to all paths
basePath: /api/v1/FilterWheel/0
produces:
  - application/json
paths:
  /Description:
    get:
      summary: Device description
      description: The description of the  filter wheel device
      responses:
        200:
          description: A string description of the device
          schema:
              $ref: '#/definitions/StringResponse'
  /Name:
    get:
      summary: Device name
      description: The name of the filter wheel device
      responses:
        200:
          description: A string name of the device
          schema:
              $ref: '#/definitions/StringResponse'
definitions:
  StringResponse:
    type: object
    properties:
      ClientTransactionID:
        type: integer
        format: int32
        description: Client's transaction ID.
      ServerTransactionID:
        type: integer
        format: int32
        description: Server's transaction ID.
      Method:
        type: string
        description: Name of the calling method.
      ErrorNumber:
        type: integer
        format: int32
        description: Error number from device.
      ErrorMessage:
        type: string
        description: Error message description from device.
      DriverException:
        type: object
        description: Windows error exception object (only of value if client is Windows in which case deserialise to a .NET Exception object).
      Value:
        type: string
        description: String response from the device.
#  Error:
#    type: string
#    description: Error description
  