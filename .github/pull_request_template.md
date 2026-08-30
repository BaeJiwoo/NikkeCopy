## Summary

<!-- What does this PR change? Keep the first sentence clear enough to understand from the PR list. -->

## Why

<!-- Why is this change needed? Link the related issue if one exists, for example: Closes #25. -->

## Type of change

- [ ] Client gameplay, UI, input, or view navigation
- [ ] Client networking or API integration
- [ ] Server API, authentication, or application logic
- [ ] Database, persistence, or migration
- [ ] Shared contract or protocol
- [ ] Assets, content, balancing, or tuning
- [ ] Build, tooling, docs, or maintenance

## Player-facing impact

<!-- Describe changes to UI, controls, progression, balance, performance, or the player's experience. Use "None" if this is internal-only. -->

## API and data impact

<!-- Fill in affected rows. Use "N/A" for rows that do not apply. -->

- API endpoints or contracts:
- Authentication or authorization:
- Database schema or migrations:
- Save data or client preferences:
- Backward compatibility:

## Technical notes

<!-- Mention important implementation details, tradeoffs, migration notes, or reviewer focus areas. -->

- Unity client impact:
- ASP.NET Core server impact:
- Infrastructure impact:

## How was this tested?

<!-- List commands, platforms, manual scenarios, and any intentionally skipped checks. -->

- [ ] Unity scripts compiled successfully
- [ ] .NET restore and build succeeded
- [ ] Automated tests passed, or missing tests are explained below
- [ ] Changed client behavior was manually tested
- [ ] Client/server integration was checked, if affected
- [ ] Authentication and authorization were checked, if affected
- [ ] Database migrations were checked, if affected
- [ ] Performance was considered for frequently executed or networked behavior

Test details:

```text
<!-- Example:
dotnet build
dotnet test
Manual: logged in through AuthView and verified navigation to MainView.
-->
```

## Screenshots or recordings

<!-- Add screenshots, GIFs, or short recordings for visual or gameplay changes. Use "N/A" for non-visual changes. -->

## Checklist

- [ ] Documentation or README changes were added, if needed
- [ ] New dependencies, tools, assets, or configuration changes are documented
- [ ] Generated files, local-only settings, secrets, and credentials were not introduced
- [ ] Breaking changes, data migrations, or protocol changes are clearly described
- [ ] Review notes are included for risky or non-obvious changes
