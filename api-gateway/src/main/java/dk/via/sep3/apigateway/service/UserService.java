package dk.via.sep3.apigateway.service;

import dk.via.sep3.apigateway.dto.UserDTO;

import java.util.List;

public interface UserService
{
  UserDTO addUser(UserDTO user);
  List<UserDTO> getAllUsers();
}
