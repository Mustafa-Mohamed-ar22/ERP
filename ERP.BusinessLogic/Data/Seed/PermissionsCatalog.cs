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

        ("purchasing.orders.manage", "Purchasing", "Manage purchase orders"),

        ("inventory.stock.view", "Inventory", "View stock levels"),
        ("inventory.stock.adjust", "Inventory", "Adjust stock"),

        // Accounting
        ("accounting.accounts.view", "Accounting", "View chart of accounts"),
        ("accounting.accounts.manage", "Accounting", "Manage chart of accounts"),

        ("accounting.journal.view", "Accounting", "View journal entries"),
        ("accounting.journal.create", "Accounting", "Create journal entries"),
        ("accounting.journal.post", "Accounting", "Post journal entries"),
        ("accounting.journal.reverse", "Accounting", "Reverse posted journal entries"),

        // Add more as each module comes online
    };
}