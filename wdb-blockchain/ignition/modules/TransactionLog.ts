import { buildModule } from "@nomicfoundation/hardhat-ignition/modules";

export default buildModule("TransactionLog", (m) => {
    const transactionLog = m.contract("TransactionLog");
    return { transactionLog };
});