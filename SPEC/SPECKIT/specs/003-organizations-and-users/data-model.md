# Data Model: Organizations and Users

## Organization
- `organizationId`
- `companyName`
- `address`
- `city`
- `state`
- `zip`
- `phone`
- `primaryContactFirstName`
- `primaryContactLastName`
- `logoUrl` nullable
- `logoThumbnailUrl` nullable
- `logoHeightPx` nullable
- `isArchived`

## User
- `userId`
- `organizationId` nullable for Site Admin
- `firstName`
- `lastName`
- `email`
- `role`
- `status`
