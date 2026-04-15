@{
    Rules = @{
        PSAvoidUsingCmdletAliases = @{
            AllowList = @('foreach', 'measure', 'select', 'where')
        }
    }
}