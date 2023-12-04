

# Baby_NI_Project_BE_YUVO

On the Back-end, Services with there Interfaces are created ParserService, responsible for detecting and parsing telco data files based on provided specifications; LoaderService, connecting to a Vertica database to load parsed CSV files into designated tables; and AggregatorService, detecting newly loaded data and aggregating it into hourly and daily Key Performance Indicators (KPIs). These KPIs include RSL_DEVIATION, RSL_INPUT_POWER, and MAX_RX_LEVEL, as outlined in the ISD document. The system ensures logging for parsing, loading, and aggregating processes and allows for re-execution if needed(found as bool in WatcherService class, In OnCreated Function), Inaddition to another project(Baby_NI RetrieveData) created for the FetchService purpose.

## Installation

Clone the GitHub repository:
```bash
git clone (https://github.com/sallyharf28/Baby_NI_Project.git)

```
Run the Project after cloning:

```bash
Startup Project
```
open the project solution file (.sln) using the IDE. //(If installed as zip file)

## Usage
Connection to Vertica Database details:
```bash
    Server=10.10.4.231;
    Database=test;
    User=bootcamp4;
    Password=bootcamp42023

    #Created Tables in the Database:
    1- FileMetadata -> To save the Created/Uploaded file Names, date of Modification, Reparseing state in a table in the Database
    2-TRANS_MW_ERC_PM_TN_RADIO_LINK_POWER -> Contains all the enabled fields in the Radio_Link_Power file 
    3-TRANS_MW_ERC_PM_WAN_RFINPUTPOWER -> Contains all the enabled fields in the RF_InputPower file
    4-TRANS_MW_AGG_SLOT_DAILY -> Contains the Aggregated Daily data
    5-TRANS_MW_AGG_SLOT_HOURLY -> Conatins the Aggregated Hourly data
```

## Steps

Database Tables:
```bash
CREATE TABLE FileMetadata (
    FileName VARCHAR(255),
    LastModifiedDate TIMESTAMP,
    Reparsed VARCHAR(255)
);

```
```bash
CREATE TABLE TRANS_MW_ERC_PM_TN_RADIO_LINK_POWER (
    NETWORK_SID INT,
    DATETIME_KEY DATETIME,
    NEID FLOAT,
    "OBJECT" VARCHAR(70),
    "TIME" DATETIME,
    "INTERVAL" INT,
    DIRECTION VARCHAR(70),
    NEALIAS VARCHAR(70),
    NETYPE VARCHAR(70),
    RXLEVELBELOWTS1 INT,
    RXLEVELBELOWTS2 INT,
    MINRXLEVEL FLOAT,
    MAXRXLEVEL FLOAT,
    TXLEVELABOVETS1 INT,
    MINTXLEVEL FLOAT,
    MAXTXLEVEL FLOAT,
    FAILUREDESCRIPTION VARCHAR(70),
    LINK VARCHAR(70),
    TID VARCHAR(70),
    FARENDTID VARCHAR(70),
    SLOT INT,
    PORT INT
)

```

```bash    
CREATE TABLE TRANS_MW_ERC_PM_WAN_RFINPUTPOWER (
    NETWORK_SID INT,
    DATETIME_KEY DATETIME,
    NODENAME VARCHAR(70),
    NEID FLOAT,
    "OBJECT" VARCHAR(70),
    "TIME" DATETIME,
    "INTERVAL" INT,
    DIRECTION VARCHAR(70),
    NEALIAS VARCHAR(70),
    NETYPE VARCHAR(70),
    RFINPUTPOWER FLOAT,
    TID VARCHAR(70),
    FARENDTID VARCHAR(70),
    SLOT VARCHAR(70),
    PORT VARCHAR(70)
)
```

```bash    
CREATE TABLE TRANS_MW_AGG_SLOT_HOURLY (
       "TIME" DATETIME,
       DATETIME_KEY DATETIME,
       NETWORK_SID INT,
       NEALIAS  VARCHAR(70),
       NETYPE VARCHAR(70),
       RSL_INPUT_POWER FLOAT,
       MAX_RX_LEVEL FLOAT,
       RSL_DEVIATION FLOAT
)
```
```bash    
CREATE TABLE TRANS_MW_AGG_SLOT_DAILY(
      "TIME" DATETIME,  
      DATETIME_KEY DATETIME,
      NETWORK_SID INT,
      NEALIAS VARCHAR(70),
      NETYPE VARCHAR(70),
      RSL_INPUT_POWER FLOAT,
      MAX_RX_LEVEL FLOAT,
      RSL_DEVIATION FLOAT
)
```

To ensure the proper functioning of the frontend, it is essential to run the Baby_NI_RetrieveData project beforehand.

