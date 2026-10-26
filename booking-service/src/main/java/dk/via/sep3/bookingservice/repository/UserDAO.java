package dk.via.sep3.bookingservice.repository;

import dk.via.sep3.bookingservice.model.User;
import org.springframework.stereotype.Repository;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.StandardOpenOption;
import java.util.ArrayList;
import java.util.List;

@Repository public class UserDAO
{
  // our "database": one user per line -> id;name;email;role
  private final Path file = Path.of("users.txt");

  // get all users from the file
  public synchronized List<User> getAllUsers()
  {
    List<User> users = new ArrayList<>();
    if (!Files.exists(file))
    {
      return users;                       // no file yet = no users yet
    }
    try
    {
      for (String line : Files.readAllLines(file))
      {
        if (line.isBlank())
          continue;
        String[] parts = line.split(";");
        users.add(new User(Integer.parseInt(parts[0]), parts[1], parts[2],
            parts[3]));
      }
    }
    catch (IOException e)
    {
      throw new RuntimeException("Could not read users.txt", e);
    }
    return users;
  }

  // add a user at the end of the file
  public synchronized User addUser(String name, String email, String role)
  {
    int id = getAllUsers().size() + 1;
    User user = new User(id, name, email, role);
    String line =
        id + ";" + name + ";" + email + ";" + role + System.lineSeparator();
    try
    {
      Files.writeString(file, line, StandardOpenOption.CREATE,
          StandardOpenOption.APPEND);
    }
    catch (IOException e)
    {
      throw new RuntimeException("Could not write to users.txt", e);
    }
    return user;
  }
}