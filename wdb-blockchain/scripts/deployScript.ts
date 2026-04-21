import { ethers } from "hardhat";
import * as fs from "fs";
import * as path from "path";

async function main() {
    console.log("Deploying TransactionLog contract..");

    const [deployer] = await ethers.getSigners();
    console.log("Deloying with account:", deployer.address);

    //deploying contract

    // get compiled contract
    const TransactionLog = await ethers.getContractFactory("TransactionLog");

    //deploy to the node
    const contract = await TransactionLog.deploy();
    await contract.waitForDeployment();

    const address = await contract.getAddress();
    console.log("TransactionLog deployed to:", address);

    //appsettings.json location
    const appSettingsPath = path.join(
        process.cwd(),
        "../wdb-backend/appsettings.json"
    );

    //check if appsettings file exist
    if (!fs.existsSync(appSettingsPath)) {
        console.error("appsettings.json in wdb-backend not found at:", appSettingsPath);
        process.exit(1);
    }

    // read current appsettings.json
    const appSettings = JSON.parse(
        fs.readFileSync(appSettingsPath, "utf8")
    );



    //update contract address
    appSettings.Blockchain.ContractAddress = address;
    appSettings.Blockchain.AbiPath = "../wdb-blockchain/artifacts/contracts/TransactionLog.sol/TransactionLog.json";

    //write to appSettings.json
    fs.writeFileSync(
        appSettingsPath,
        JSON.stringify(appSettings, null, 2)
    );

    console.log("appsettings.json updated");
    console.log("New contractAdress:", address);
    console.log("Restart your backend to apply changes");
}

main().catch((error) => {
    console.error(error);
    process.exitCode = 1;
});