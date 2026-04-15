@{
    Rules = @{
        PSAvoidUsingCmdletAliases = @{
            AllowList = @('foreach', 'measure', 'select', 'sort', 'where')
        }
    }
}
