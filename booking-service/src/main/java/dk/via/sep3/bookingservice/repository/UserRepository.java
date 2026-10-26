package dk.via.sep3.bookingservice.repository;

import dk.via.sep3.bookingservice.model.User;
import org.springframework.data.jpa.repository.JpaRepository;

public interface UserRepository extends JpaRepository<User, Integer> {
  // findAll(), save(), findById()... are already included
}