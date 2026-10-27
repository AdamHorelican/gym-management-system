package dk.via.sep3.bookingservice.repository;

import dk.via.sep3.bookingservice.model.User;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Repository;

import java.util.List;

@Repository
public class UserDAO {
  private final UserRepository userRepository;

  @Autowired
  public UserDAO(UserRepository userRepository) {
    this.userRepository = userRepository;
  }

  // SELECT * FROM users
  public List<User> getAllUsers() {
    return userRepository.findAll();
  }

  // INSERT INTO users ... and return the saved user with its new id
  public User addUser(String name, String email, String role) {
    return userRepository.save(new User(null, name, email, role));
  }
}