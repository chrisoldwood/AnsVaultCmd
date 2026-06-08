@{
    Rules = @{
        ExcludeRules=@('PSAvoidUsingWriteHost')
        # Don't allow 'sort' as it doesn't work on Linux.
        PSAvoidUsingCmdletAliases = @{
            AllowList = @('foreach', 'measure', 'select', 'where')
        }
    }
}
