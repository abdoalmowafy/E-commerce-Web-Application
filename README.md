# Full Stack Web Store App Using ASP.NET Core (MVC)

## Overview
This project is a comprehensive web store application built with ASP.NET Core (MVC). It incorporates several advanced features to provide a robust and user-friendly experience for both customers and administrators.

## Features

- **Identity Framework**: Utilizes a modified User model to manage user authentication and authorization.
- **External Logins**: Supports authentication through Facebook, Google, and Microsoft accounts.
- **Payment Gateway**: Integrates with Paymob to support credit card and mobile wallet payments.
- **Manager Interface**: A control panel for managers to add, edit, or delete products and orders. It also includes visual charts to track each product's performance.
- **Job Application Interface**: Allows candidates to apply for positions such as "Moderator", "Transporter", etc., by submitting their CVs. (Soon!)
- **Role-Based Interfaces**: Provides specialized interfaces tailored to each role within the store.

## Installation

1. **Clone the repository**:
    ```bash
    git clone https://github.com/abdoalmowafy/Egost.git
    ```

2. **Navigate to the project directory**:
    ```bash
    cd Egost
    ```

3. **Restore dependencies**:
    ```bash
    dotnet restore
    ```

4. **Create a `.env` File**: Create a `.env` file in the root of your project to store the environment variables.

5. **Add Environment Variables**: Add the necessary environment variables to the `.env` file.

    ```env
    SuperUser__Password=...
    ConnectionStrings__DefaultConnection=...

    Authentication__Google__ClientId=...
    Authentication__Google__ClientSecret=...

    Authentication__Facebook__AppId=...
    Authentication__Facebook__AppSecret=...

    Authentication__Microsoft__ClientId=...
    Authentication__Microsoft__ClientSecret=...

    Paymob__ApiKey=...
    Paymob__SecretKey=...
    Paymob__PublicKey=...
    Paymob__HMAC=...
    Paymob__IntegrationId=...
    Paymob__Iframe1Id=...
    Paymob__Iframe2Id=...
    ```

6. **Run the application**:
    ```bash
    dotnet run
    ```

## Usage

- **Manager Interface**: Accessible to users with manager roles, allowing for product and order management, and viewing performance charts.
- **Job Application**: Candidates can apply for various roles within the store through a dedicated interface.
- **Role-Based Access**: Each user role within the store has a specialized interface designed to provide relevant functionality.

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request for any improvements or bug fixes.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Contact

For any inquiries or support, please contact [abdoalmowafy@gmail.com](mailto:abdoalmowafy@gmail.com).
