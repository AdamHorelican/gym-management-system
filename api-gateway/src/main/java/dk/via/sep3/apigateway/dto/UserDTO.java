package dk.via.sep3.apigateway.dto;

// what the REST API sends and receives as JSON
public record UserDTO(Integer id, String name, String email, String role) {
}