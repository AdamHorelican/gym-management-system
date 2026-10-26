package dk.via.sep3.apigateway.config;

import dk.via.sep3.generated.UserServiceGrpc;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.grpc.client.GrpcChannelFactory;

@Configuration
public class GrpcClientConfig {
  @Value("${booking.service.grpc.address}")     // read from application.properties
  private String bookingServiceAddress;

  // creates the gRPC client (stub) once, so it can be injected everywhere
  @Bean
  public UserServiceGrpc.UserServiceBlockingStub userServiceStub(GrpcChannelFactory channels) {
    return UserServiceGrpc.newBlockingStub(channels.createChannel(bookingServiceAddress));
  }
}