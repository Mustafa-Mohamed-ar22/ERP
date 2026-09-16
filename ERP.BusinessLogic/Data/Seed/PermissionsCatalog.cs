public static class PermissionsCatalog
{
    public static readonly (string Code, string Module, string Description)[] All =
    {
        ("core.companies.manage", "Core", "Manage companies"),
        ("core.branches.manage", "Core", "Manage branches"),
        ("core.users.manage", "Core", "Manage users"),
        ("core.roles.manage", "Core", "Manage roles and permissions"),
        ("core.departments.manage", "Core", "Manage departments"),

        ("hr.employees.view", "HR", "View employees"),
        ("hr.employees.manage", "HR", "Create/edit employees"),

        ("sales.invoices.view", "Sales", "View sales invoices"),
        ("sales.invoices.create", "Sales", "Create sales invoices"),
        ("sales.invoices.approve", "Sales", "Approve sales invoices"),

        

        // Accounting
        ("accounting.accounts.view", "Accounting", "View chart of accounts"),
        ("accounting.accounts.manage", "Accounting", "Manage chart of accounts"),
        ("accounting.journal.view", "Accounting", "View journal entries"),
        ("accounting.journal.create", "Accounting", "Create journal entries"),
        ("accounting.journal.post", "Accounting", "Post journal entries"),
        ("accounting.journal.reverse", "Accounting", "Reverse posted journal entries"),
        ("accounting.settings.manage", "Accounting", "Configure default GL accounts for automatic postings"),


        // inventory
        ("inventory.stock.view", "Inventory", "View stock levels"),
        ("inventory.stock.adjust", "Inventory", "Adjust stock"),
        ("inventory.products.view", "Inventory", "View products"),
        ("inventory.products.manage", "Inventory", "Manage products"),
        ("inventory.warehouses.view", "Inventory", "View warehouses"),
        ("inventory.warehouses.manage", "Inventory", "Manage warehouses"),
        ("inventory.stock.transfer", "Inventory", "Transfer stock between warehouses"),
        ("inventory.categories.manage", "Inventory", "Manage product categories"),



        // purchasing
        ("purchasing.suppliers.view", "Purchasing", "View suppliers"),
        ("purchasing.suppliers.manage", "Purchasing", "Manage suppliers"),
        ("purchasing.orders.view", "Purchasing", "View purchase orders"),
        ("purchasing.orders.create", "Purchasing", "Create and edit draft purchase orders"),
        ("purchasing.orders.approve", "Purchasing", "Approve purchase orders"),
        ("purchasing.orders.receive", "Purchasing", "Receive goods against purchase orders"),
        ("purchasing.orders.cancel", "Purchasing", "Cancel purchase orders"),
        ("purchasing.orders.manage", "Purchasing", "Manage purchase orders"),


        // Sales
        ("sales.customers.view", "Sales", "View customers"),
        ("sales.customers.manage", "Sales", "Manage customers"),
        ("sales.orders.view", "Sales", "View sales orders"),
        ("sales.orders.create", "Sales", "Create and edit draft sales orders"),
        ("sales.orders.approve", "Sales", "Approve sales orders"),
        ("sales.orders.ship", "Sales", "Ship goods against sales orders"),
        ("sales.orders.cancel", "Sales", "Cancel sales orders"),


        // HR
        ("hr.leaves.view", "HR", "View leave requests"),
        ("hr.leaves.request", "HR", "Submit and cancel own leave requests"),
        ("hr.leaves.approve", "HR", "Approve or reject leave requests"),
        ("hr.attendance.view", "HR", "View attendance records"),
    };
}