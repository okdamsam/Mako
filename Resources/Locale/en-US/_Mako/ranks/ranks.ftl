## Rank Examine Text
rank-examine-text = {GENDER($entity) ->
    [male] He
    [female] She
    [neuter] They
   *[other] They
} {GENDER($entity) ->
    [male] is
    [female] is
   *[other] are
} a {$rank}.

## Rank Command Messages
rank-command-set-success = Set rank of {$entity} to {$rank}.
rank-command-remove-success = Removed rank from {$entity}.
rank-command-get = Current rank: {$rank}
rank-command-get-none = Entity has no rank assigned.
rank-command-list-header = Available Ranks:
rank-command-invalid-rank = Invalid rank ID: {$rankId}

## Rank Categories
rank-category-enlisted = Enlisted
rank-category-warrant = Warrant Officer
rank-category-officer = Officer
rank-category-flag = Flag Officer
