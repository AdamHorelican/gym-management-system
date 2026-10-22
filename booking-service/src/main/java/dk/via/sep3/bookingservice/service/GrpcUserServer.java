package dk.via.sep3.bookingservice.service;

import dk.via.sep3.bookingservice.model.User;
import dk.via.sep3.generated.AddUserRequest;
import dk.via.sep3.generated.GetAllUsersRequest;
import dk.via.sep3.generated.UserListResponse;
import dk.via.sep3.generated.UserResponse;
import dk.via.sep3.generated.UserServiceGrpc;
import io.grpc.stub.StreamObserver;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.grpc.server.service.GrpcService;

@GrpcService
public class GrpcUserServer extends UserServiceGrpc.UserServiceImplBase {
  private final UserService userService;

  @Autowired
  public GrpcUserServer(UserService userService) {
    this.userService = userService;
  }

  // rpc AddUser
  @Override
  public void addUser(AddUserRequest request, StreamObserver<UserResponse> responseObserver) {
    User user = userService.addUser(request.getName(), request.getEmail(), request.getRole()); // 1. call service
    UserResponse response = UserResponse.newBuilder()
        .setUser(UserMapper.toProto(user))                                                 // 2. map model -> proto
        .build();
    responseObserver.onNext(response);                                                         // 3. send
    responseObserver.onCompleted();                                                            // 4. finish
  }

  // rpc GetAllUsers
  @Override
  public void getAllUsers(GetAllUsersRequest request, StreamObserver<UserListResponse> responseObserver) {
    UserListResponse.Builder builder = UserListResponse.newBuilder();
    for (User user : userService.getAllUsers()) {
      builder.addUsers(UserMapper.toProto(user));
    }
    responseObserver.onNext(builder.build());
    responseObserver.onCompleted();
  }

  // converts OUR User (model) -> GENERATED User (protobuf), written with its full name
  private static class UserMapper {
    static dk.via.sep3.generated.User toProto(User u) {
      return dk.via.sep3.generated.User.newBuilder()
          .setId(u.getId())
          .setName(u.getName())
          .setEmail(u.getEmail())
          .setRole(u.getRole())
          .build();
    }
  }
}