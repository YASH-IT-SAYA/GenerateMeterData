## 🔧 Data Handling (Development & Testing Environments)

### ✅ Update Existing Records
In the **development** and **testing** environments, the following tables support updating existing values such as `meternumber`, `gatewaynumber`, and `Date`.  
These updates are performed **in memory** and will only be written to the database when explicitly saved.

- `WaterMeterFlowReportLatest`
- `WaterMeterStatusReportLatest`
- `DailyMeterViseCons`

### ➕ Insert New Records
New records can be directly inserted into the following tables across **both environments**:

- `consumption_disaggregation`
- `WaterClassifySummary`

> ⚠️ Ensure the correct **connection string** (for development or testing) is selected before saving data to the database.
