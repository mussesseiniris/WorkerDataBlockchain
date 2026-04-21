import { expect } from "chai";
import hre from "hardhat";

//test for loggin transactions
describe("TransactionLog", function () {

    const Action = {
        PermissionRequested: 0,
        PermissionApproved: 1,
        PermissionRejected: 2,
        DataViewed: 3,
        PermissionRevoked: 4
    };

    //deploy new contract for test
    async function deployFixture() {
        const [employer, worker, otherEmployer, otherWorker] =
            await hre.ethers.getSigners();

        const TransactionLog =
            await hre.ethers.getContractFactory("TransactionLog");

        const contract = await TransactionLog.deploy();

        return { contract, employer, worker, otherEmployer, otherWorker };
    }

    //test on deployment. Check deployment will create contract address. Shall not be empty
    describe("Deployment", function () {

        it("Deploy and have a valid contract address", async function () {
            const { contract } = await deployFixture();
            expect(await contract.getAddress()).to.not.be.empty;
        });

    });

    describe("Transactions", function () {

        it("Test for PermissionRequest", async function () {
            const { contract, employer, worker } = await deployFixture();
            const timestamp = Math.floor(Date.now() / 1000);

            await expect(
                contract.logTransaction(
                    employer.address,
                    worker.address,
                    timestamp,
                    Action.PermissionRequested
                )
            )
                .to.emit(contract, "TransactionLogged")
                .withArgs(
                    employer.address,
                    worker.address,
                    timestamp,
                    Action.PermissionRequested
                );
        });

        //test for PermissionApproved
        it("Test for PermissionApproved", async function () {
            const { contract, employer, worker } = await deployFixture();
            const timestamp = Math.floor(Date.now() / 1000);

            await expect(
                contract.logTransaction(
                    employer.address,
                    worker.address,
                    timestamp,
                    Action.PermissionApproved
                )
            )
                .to.emit(contract, "TransactionLogged")
                .withArgs(
                    employer.address,
                    worker.address,
                    timestamp,
                    Action.PermissionApproved
                );
        });

        //test for PermissionRejected
        it("Test for PermissionRejected", async function () {
            const { contract, employer, worker } = await deployFixture();
            const timestamp = Math.floor(Date.now() / 1000);

            await expect(
                contract.logTransaction(
                    employer.address,
                    worker.address,
                    timestamp,
                    Action.PermissionRejected
                )
            )
                .to.emit(contract, "TransactionLogged")
                .withArgs(
                    employer.address,
                    worker.address,
                    timestamp,
                    Action.PermissionRejected
                );
        });

        //test for DataViewed
        it("Test for DataViewed", async function () {
            const { contract, employer, worker } = await deployFixture();
            const timestamp = Math.floor(Date.now() / 1000);

            await expect(
                contract.logTransaction(
                    employer.address,
                    worker.address,
                    timestamp,
                    Action.DataViewed
                )
            )
                .to.emit(contract, "TransactionLogged")
                .withArgs(
                    employer.address,
                    worker.address,
                    timestamp,
                    Action.DataViewed
                );
        });

        //test for PermissionRevoked
        it("Test for PermissionRevoked", async function () {
            const { contract, employer, worker } = await deployFixture();
            const timestamp = Math.floor(Date.now() / 1000);

            await expect(
                contract.logTransaction(
                    employer.address,
                    worker.address,
                    timestamp,
                    Action.PermissionRevoked
                )
            )
                .to.emit(contract, "TransactionLogged")
                .withArgs(
                    employer.address,
                    worker.address,
                    timestamp,
                    Action.PermissionRevoked
                );
        });

    });

});