package dk.via.sep3.bookingservice.service;

import dk.via.sep3.bookingservice.model.User;

import java.util.List;

public interface UserService
{
  User addUser(String name, String email, String role);
  List<User> getAllUsers();
}
