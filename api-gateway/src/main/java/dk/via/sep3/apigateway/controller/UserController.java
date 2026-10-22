package dk.via.sep3.apigateway.controller;

import dk.via.sep3.apigateway.dto.UserDTO;
import dk.via.sep3.apigateway.service.UserService;
import io.grpc.StatusRuntimeException;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.servlet.support.ServletUriComponentsBuilder;

import java.net.URI;
import java.util.List;

@CrossOrigin("*")
@RestController
@RequestMapping("/api/v1/users")
public class UserController {
  private final UserService userService;

  @Autowired
  public UserController(UserService userService) {
    this.userService = userService;
  }

  @GetMapping("/all")
  public ResponseEntity<List<UserDTO>> getAllUsers() {
    try {
      return ResponseEntity.ok(userService.getAllUsers());
    } catch (StatusRuntimeException e) {          // Booking Service could not be reached
      return ResponseEntity.status(HttpStatus.SERVICE_UNAVAILABLE).build();
    }
  }

  @PostMapping("/")
  public ResponseEntity<UserDTO> addUser(@RequestBody UserDTO user) {
    if (user.name() == null || user.email() == null) {
      return ResponseEntity.badRequest().build();   // 400
    }
    try {
      UserDTO created = userService.addUser(user);
      URI location = ServletUriComponentsBuilder.fromCurrentRequest()
          .path("{id}").buildAndExpand(created.id()).toUri();
      return ResponseEntity.created(location).body(created);   // 201 + the new user
    } catch (StatusRuntimeException e) {
      return ResponseEntity.status(HttpStatus.SERVICE_UNAVAILABLE).build();   // 503
    }
  }
}