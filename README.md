# TriplePrime

TriplePrime is a .NET 8 solution that consists of three main projects: API, Data, and Infrastructure. This solution is designed to provide a robust architecture for building scalable and maintainable applications.

## Project Structure

- **TriplePrime.API**: This project contains the web API that handles incoming HTTP requests and returns responses. It includes controllers, configuration settings, and the main entry point for the application.
  
- **TriplePrime.Data**: This project is responsible for data access. It includes entity classes, repositories for data manipulation, and the database context for managing database interactions.

- **TriplePrime.Infrastructure**: This project contains the business logic and services that interact with the Data project. It defines interfaces for services to facilitate dependency injection and testing.

## Getting Started

To get started with the TriplePrime solution, follow these steps:

1. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd TriplePrime
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Run the API**:
   ```bash
   dotnet run --project src/TriplePrime.API/TriplePrime.API.csproj
   ```

4. **Run tests**:
   To run the unit tests for each project, use the following command:
   ```bash
   dotnet test
   ```

## Contributing

Contributions are welcome! Please feel free to submit a pull request or open an issue for any enhancements or bug fixes.

## License

This project is licensed under the MIT License. See the LICENSE file for more details.