# Grocery Management & Billing System - Specification

## 1. Project Overview

- **Project Name**: GroceryStore
- **Type**: Cross-platform Desktop Application (Avalonia .NET)
- **Core Functionality**: A comprehensive grocery management and billing system that handles inventory management, product catalog, billing/POS operations, and sales reporting.
- **Target Users**: Small to medium grocery store owners and cashiers

## 2. UI/UX Specification

### 2.1 Layout Structure

**Window Model**:
- Single main window with navigation sidebar
- Modal dialogs for: Add/Edit Product, Add/Edit Category, Confirm Delete, Billing Receipt
- Window size: 1280x800 (minimum), resizable

**Layout Areas**:
- **Sidebar** (Left, 220px width): Navigation menu with icons
- **Header** (Top, 60px height): App title, current date/time, user info
- **Content Area** (Center): Dynamic content based on selected menu
- **Status Bar** (Bottom, 30px height): Quick stats (total products, today's sales)

### 2.2 Visual Design

**Color Palette**:
- Primary: #2E7D32 (Forest Green - represents fresh produce)
- Primary Light: #4CAF50
- Primary Dark: #1B5E20
- Secondary: #FF6F00 (Amber - for accents and CTAs)
- Background: #F5F5F5
- Surface: #FFFFFF
- Text Primary: #212121
- Text Secondary: #757575
- Error: #D32F2F
- Success: #388E3C
- Border: #E0E0E0

**Typography**:
- Font Family: Segoe UI, Arial (system default)
- Headings H1: 24px, SemiBold
- Headings H2: 20px, SemiBold
- Headings H3: 16px, Medium
- Body: 14px, Regular
- Small: 12px, Regular

**Spacing System**:
- Base unit: 8px
- Margins: 8px, 16px, 24px, 32px
- Padding: 8px, 12px, 16px, 24px
- Border radius: 4px (buttons), 8px (cards)

**Visual Effects**:
- Card shadows: 0 2px 4px rgba(0,0,0,0.1)
- Hover states: slight background color change
- Transitions: 150ms ease-in-out

### 2.3 Components

**Navigation Items**:
- Dashboard (Home icon)
- Products (Box icon)
- Categories (Folder icon)
- Billing (Calculator icon)
- Reports (Chart icon)
- Settings (Gear icon)

**Common Components**:
- DataGrid: For product/category lists with sorting
- TextBox: For input fields
- ComboBox: For category selection
- NumericUpDown: For quantity/price inputs
- Button: Primary (green), Secondary (gray), Danger (red)
- Card: For dashboard stats
- Dialog: Modal windows for CRUD operations

**Component States**:
- Default: Normal appearance
- Hover: Slightly darker background
- Active/Pressed: Even darker
- Disabled: 50% opacity, no interaction
- Focus: Blue outline ring

## 3. Functional Specification

### 3.1 Core Features

#### Dashboard
- Display total products count
- Display total categories count
- Display today's sales amount
- Display today's transactions count
- Display low stock alerts (products with quantity < 10)
- Recent transactions list (last 10)

#### Product Management
- View all products in a DataGrid
- Columns: ID, Name, Category, Price, Quantity, Unit, Barcode, Actions
- Add new product (dialog form)
- Edit existing product
- Delete product (with confirmation)
- Search products by name/barcode
- Filter by category
- Sort by any column
- Low stock indicator (quantity < 10)

**Product Fields**:
- ID (auto-generated)
- Name (required, max 100 chars)
- Category (required, dropdown)
- Price (required, decimal, min 0)
- Quantity (required, integer, min 0)
- Unit (required: kg, g, L, mL, piece, dozen, pack)
- Barcode (optional, max 50 chars)
- Description (optional, max 500 chars)

#### Category Management
- View all categories
- Add new category
- Edit category name
- Delete category (only if no products linked)
- Display product count per category

**Category Fields**:
- ID (auto-generated)
- Name (required, max 50 chars)

#### Billing/POS
- Product search bar (search by name or barcode)
- Product list with quick-add buttons
- Cart display (table with: Product, Price, Qty, Total)
- Quantity adjustment (+/- buttons)
- Remove item from cart
- Subtotal, Tax (5%), Grand Total
- Complete Sale button
- Clear Cart button
- Generate receipt (printable format)
- Update inventory on sale completion

**Billing Flow**:
1. Search/scan product
2. Add to cart (default qty 1)
3. Adjust quantity as needed
4. Review totals
5. Click "Complete Sale"
6. Receipt shown, inventory updated
7. Cart cleared, ready for next customer

#### Reports
- Sales by date range
- Sales summary (total, average transaction)
- Top selling products
- Category-wise sales breakdown
- Export to CSV

#### Settings
- Store name configuration
- Tax rate configuration
- Currency symbol
- Low stock threshold

### 3.2 Data Flow & Architecture

**Layers**:
- **Models**: Product, Category, Transaction, TransactionItem, Settings
- **ViewModels**: DashboardViewModel, ProductsViewModel, CategoriesViewModel, BillingViewModel, ReportsViewModel, SettingsViewModel
- **Views**: MainWindow, DashboardView, ProductsView, CategoriesView, BillingView, ReportsView, SettingsView
- **Services**: DataService (JSON file storage), ReportService

**Data Storage**:
- JSON files in user's AppData folder
- Files: products.json, categories.json, transactions.json, settings.json

**Key Classes**:
- `Product`: Id, Name, CategoryId, Price, Quantity, Unit, Barcode, Description
- `Category`: Id, Name
- `Transaction`: Id, Date, Items[], TotalAmount, TaxAmount
- `TransactionItem`: ProductId, ProductName, Quantity, UnitPrice, TotalPrice

### 3.3 Edge Cases

- Empty product list: Show "No products yet" message with Add button
- Empty cart: Disable Complete Sale button
- Zero quantity product: Show "Out of Stock", prevent adding to cart
- Delete category with products: Show error message
- Invalid input: Show validation errors inline
- Search no results: Show "No products found" message

## 4. Acceptance Criteria

### Visual Checkpoints
- [ ] App launches with green sidebar navigation
- [ ] Dashboard shows 4 stat cards with icons
- [ ] Products view has searchable DataGrid
- [ ] Billing view has split layout (products left, cart right)
- [ ] All dialogs are modal and centered
- [ ] Color scheme is consistent throughout

### Functional Checkpoints
- [ ] Can add, edit, delete products
- [ ] Can add, edit, delete categories
- [ ] Can add products to cart and complete sale
- [ ] Inventory decreases after sale
- [ ] Sales are recorded in transactions
- [ ] Dashboard stats update in real-time
- [ ] Data persists after app restart
- [ ] Low stock warnings appear on dashboard
- [ ] Reports show sales data correctly

### Technical Checkpoints
- [ ] Builds successfully on Windows and Linux
- [ ] No runtime errors or crashes
- [ ] Responsive layout (minimum 1024x768)
- [ ] Data saved to JSON files correctly
