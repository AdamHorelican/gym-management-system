package dk.via.sep3.apigateway.service;

import dk.via.sep3.apigateway.dto.UserDTO;
import dk.via.sep3.generated.AddUserRequest;
import dk.via.sep3.generated.GetAllUsersRequest;
import dk.via.sep3.generated.User;
import dk.via.sep3.generated.UserListResponse;
import dk.via.sep3.generated.UserResponse;
import dk.via.sep3.generated.UserServiceGrpc;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
public class UserServiceImpl implements UserService {
  private final UserServiceGrpc.UserServiceBlockingStub userStub;

  @Autowired
  public UserServiceImpl(UserServiceGrpc.UserServiceBlockingStub userStub) {
    this.userStub = userStub;
  }

  // DTO -> gRPC request -> call Booking Service -> gRPC response -> DTO
  @Override
  public UserDTO addUser(UserDTO user) {
    AddUserRequest request = AddUserRequest.newBuilder()
        .setName(user.name())
        .setEmail(user.email())
        .setRole(user.role() == null ? "" : user.role())   // protobuf does not accept null
        .build();
    UserResponse response = userStub.addUser(request);
    return mapToDTO(response.getUser());
  }

  // get all users from the Booking Service
  @Override
  public List<UserDTO> getAllUsers() {
    UserListResponse response = userStub.getAllUsers(GetAllUsersRequest.newBuilder().build());
    return response.getUsersList().stream()
        .map(this::mapToDTO)
        .toList();
  }

  // manual mapping
  private UserDTO mapToDTO(User user) {
    return new UserDTO(user.getId(), user.getName(), user.getEmail(), user.getRole());
  }
}