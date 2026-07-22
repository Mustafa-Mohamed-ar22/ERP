public static class PermissionsCatalog{
    public static readonly (string Code, string Module, string Description)[] All =
    {
        ("core.companies.manage", "Core", "Manage companies"),
        ("core.branches.manage", "Core", "Manage branches"),
        ("core.users.manage", "Core", "Manage users"),
        ("core.roles.manage", "Core", "Manage roles and permissions"),
        ("hr.employees.view", "HR", "View employees"),
        ("hr.employees.manage", "HR", "Create/edit employees"),
        ("sales.invoices.view", "Sales", "View sales invoices"),
        ("sales.invoices.create", "Sales", "Create sales invoices"),
        ("sales.invoices.approve", "Sales", "Approve sales invoices"),
        ("purchasing.orders.manage", "Purchasing", "Manage purchase orders"),
        ("inventory.stock.view", "Inventory", "View stock levels"),
        ("inventory.stock.adjust", "Inventory", "Adjust stock"),
        ("accounting.journal.post", "Accounting", "Post journal entries"),
        ("core.departments.manage", "Core", "Manage departments"),
        // add more as each module comes online
    };
}