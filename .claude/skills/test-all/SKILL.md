# Run Full Test Suite

รัน Unit Tests, Integration Tests, และ Manual Checklist

## Usage

```
/test-all
```

## Steps

1. Build solution: `dotnet build`
2. Run unit tests: `dotnet test --filter Category=Unit`
3. Run integration tests: `dotnet test --filter Category=Integration`
4. แสดงสรุปผล test
5. ถ้ามี test fail แสดง error details
6. แนะนำว่าควร manual test อะไรต่อ (จาก Manual Testing Checklist ใน CLAUDE.md)

## Manual Testing Checklist (แสดงหลัง automated tests)

- [ ] ยื่นเรื่องทั่วไป → ได้ SMS/Email
- [ ] ยื่นเรื่องทุจริต → ชื่อถูก Mask
- [ ] Auto-assign ทำงาน (รอ X ชั่วโมง)
- [ ] API Key validation ทุก case
- [ ] Rate Limiting block request ที่เกิน
- [ ] eDOC PDF export ถูกต้อง
