# Create Database Migration

สร้าง EF Core Migration พร้อมตรวจสอบความถูกต้อง

## Usage

```
/create-migration MigrationName
```

Example: `/create-migration AddApiKeysTable`

## Steps

1. ตรวจสอบว่า DbContext มีการเปลี่ยนแปลง Model
2. Run: `dotnet ef migrations add {MigrationName} --project SRT.Complaint`
3. ตรวจสอบไฟล์ Migration ที่ถูกสร้าง
4. Review `Up()` และ `Down()` methods
5. ตรวจสอบว่าไม่มี data loss
6. แสดง command สำหรับ apply migration: `dotnet ef database update`

## Pre-check

- [ ] Model changes committed?
- [ ] DbSet added to DbContext?
- [ ] Foreign keys correct?
- [ ] Index defined?

## Post-check

- [ ] Migration file created?
- [ ] No compilation errors?
- [ ] Migration reversible (Down method)?
