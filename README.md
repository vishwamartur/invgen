# InvGen - Professional Quotation Generator

A comprehensive web application designed specifically for plumbing and electrical contractors to create professional quotations and estimates.

## 🚀 Recent Updates (Latest Commit)

### ✅ **Quotation Save Functionality Fixed**
- **Resolved JavaScript Conflicts**: Fixed conflicting event handlers between `quotation-create.js` and `quotation-save-actions.js`
- **Enhanced Form Validation**: Improved validation logic with better error handling and user feedback
- **AJAX Form Submission**: Unified form submission system preventing traditional/AJAX conflicts
- **Visual Feedback**: Added comprehensive validation feedback and visual indicators for required fields
- **Real-time Monitoring**: Added MutationObserver for dynamic line items monitoring
- **Better Error Messages**: Specific validation messages for customer, date, and line item requirements
- **PDF Integration**: Fixed PDF download functionality in quotations management interface
- **Debug Capabilities**: Enhanced debugging with detailed console logging

### 🎯 **Save Button Requirements**
To successfully save a quotation, ensure:
1. **Customer Selected**: Choose from the customer dropdown
2. **Quotation Date**: Enter or select a valid date
3. **Line Items Added**: Add at least one product/service line item
4. **Complete Line Items**: Each line item must have product selection and quantity

## Features

### Core Functionality
- **Quotation Generation**: Create detailed quotations with line items, quantities, and pricing
- **Tax Calculation**: Automatic GST calculation with configurable rates (default 18%)
- **Product Catalog**: Comprehensive database of plumbing and electrical materials
- **Customer Management**: Complete customer information and history tracking
- **PDF Generation**: Professional PDF quotations ready for client presentation

### Product Catalog
- **Plumbing Materials**: Pipes, fittings, fixtures, and accessories
- **Electrical Materials**: Wires, switches, outlets, panels, and components
- **Labor Services**: Installation and service charges for both trades
- **Search & Filter**: Easy product discovery by category and service type

### Business Features
- **Multiple Quotation Status**: Draft, Sent, Approved, Rejected, Expired, Converted
- **Automatic Numbering**: Smart quotation numbering system
- **Tax Management**: Support for tax-inclusive and tax-exclusive pricing
- **Professional Layout**: Clean, modern interface suitable for business use

## Technology Stack

- **Backend**: ASP.NET Core 8.0
- **Database**: SQL Server with Entity Framework Core
- **Frontend**: Bootstrap 5, HTML5, CSS3, JavaScript
- **PDF Generation**: iTextSharp
- **API**: RESTful APIs with Swagger documentation

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB is sufficient for development)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Invgen
   ```

2. **Navigate to the web application**
   ```bash
   cd InvGen.Web
   ```

3. **Restore packages**
   ```bash
   dotnet restore
   ```

4. **Update database connection string**
   Edit `appsettings.json` and update the connection string if needed:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=InvGenDb;Trusted_Connection=true;MultipleActiveResultSets=true"
     }
   }
   ```

5. **Create and seed the database**
   ```bash
   dotnet ef database update
   ```

6. **Run the application**
   ```bash
   dotnet run
   ```

7. **Access the application**
   Open your browser and navigate to `https://localhost:5001` or `http://localhost:5000`

### Configuration

#### Company Settings
Update your company information in `appsettings.json`:
```json
{
  "CompanySettings": {
    "Name": "Your Company Name",
    "Address": "Your Company Address",
    "Phone": "Your Phone Number",
    "Email": "your.email@company.com",
    "Website": "www.yourcompany.com"
  }
}
```

#### Tax Settings
Configure default tax settings:
```json
{
  "TaxSettings": {
    "DefaultGstRate": 18.0,
    "TaxInclusiveByDefault": false
  }
}
```

## Usage

### Creating a Quotation

1. **Add Customers**: Start by adding your customers with complete contact information
2. **Manage Products**: The system comes pre-loaded with common plumbing and electrical items
3. **Create Quotation**: 
   - Select customer
   - Add line items from the product catalog
   - Set quantities and adjust pricing
   - Review tax calculations
   - Generate PDF for client

### Product Management

The system includes pre-loaded products in categories:
- **Plumbing**: Pipes (PVC, CPVC), fittings, fixtures, valves
- **Electrical**: Wires, cables, switches, sockets, panels, MCBs
- **Labor**: Installation and service charges

You can add custom products and modify existing ones as needed.

### API Documentation

When running in development mode, access the Swagger documentation at:
`https://localhost:5001/swagger`

## Project Structure

```
InvGen.Web/
├── Controllers/          # MVC Controllers
│   ├── Api/             # API Controllers
│   ├── HomeController.cs
│   ├── CustomersController.cs
│   ├── ProductsController.cs
│   └── QuotationsController.cs
├── Models/              # Data Models
├── Services/            # Business Logic Services
├── Data/                # Entity Framework Context
├── Views/               # Razor Views
├── wwwroot/             # Static Files
└── Program.cs           # Application Entry Point
```

## Database Schema

### Core Tables
- **Customers**: Customer information and contact details
- **ProductCategories**: Product categorization by service type
- **Products**: Product catalog with pricing and specifications
- **Quotations**: Quotation headers with customer and totals
- **QuotationLineItems**: Individual line items within quotations

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support and questions, please create an issue in the repository or contact the development team.

## Roadmap

- [ ] Multi-user support with authentication
- [ ] Email integration for sending quotations
- [ ] Inventory management
- [ ] Invoice generation from approved quotations
- [ ] Reporting and analytics
- [ ] Mobile app companion
- [ ] Integration with accounting software
