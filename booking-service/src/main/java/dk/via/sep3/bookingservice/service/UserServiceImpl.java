package dk.via.sep3.bookingservice.service;

import dk.via.sep3.bookingservice.model.User;
import dk.via.sep3.bookingservice.repository.UserDAO;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
public class UserServiceImpl implements UserService
{
  private final UserDAO userDAO;

  @Autowired
  public UserServiceImpl(UserDAO userDAO) {
    this.userDAO = userDAO;
  }

  @Override public User addUser(String name, String email, String role)
  {
    if (role == null || role.isBlank()){
      role = "MEMBER";
    }
    return userDAO.addUser(name, email, role);
  }

  @Override public List<User> getAllUsers()
  {
    return userDAO.getAllUsers();
  }
}
