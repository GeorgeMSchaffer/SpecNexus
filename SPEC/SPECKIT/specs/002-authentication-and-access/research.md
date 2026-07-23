# Research: Authentication and Access

## Key Decisions
- login uses globally unique email credentials
- Site Admin is global and not tenant-owned
- seeded Site Admin credential comes from environment configuration
- lockout is 5 failed attempts within 15 minutes followed by a 15-minute lockout

## Risk Areas
- protecting global Site Admin behavior from tenant assumptions
- consistent audit generation for auth outcomes
