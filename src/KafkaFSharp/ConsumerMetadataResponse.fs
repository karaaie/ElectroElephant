module ElectroElephant.ConsumerMetadataResponse

open ElectroElephant.Common

type ConsumerMetadataResponse =
  { error_code : ErrorCode 
    coordinator_id : CoordinatorId
    coordinator_host : Hostname
    coordinator_port : Port}