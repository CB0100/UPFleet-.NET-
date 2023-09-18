using System.Collections.Generic;
using UPFleet.Models;
using UPFleet.ViewModels;

namespace UPFleet.Repositories
{
    public interface IRepository
    {
        List<Barge> GetBargeList();
        List<string> GetBargeNameList(string term);
        List<Owner> GetOwnerList();
        List<Owner> GetOwnerofBargesList();
        List<Transfer> GetTransferList();
        List<Transaction> GetTransactionList();
        double? GetTransactionCount();
        List<Transaction> GetTransactionListforNotBilled();
        List<Transaction> GetTransactionListforBilled();
        List<Transaction> GetTransactionListforToBill();
        List<PeachtreeExportedArchive> GetPeachtreeExportedArchiveList();
        List<Location> GetLocationList();
        List<UPFleetViewModel> GetStatusData(string Status);
        bool AddBarge(Barge barge);
        bool AddOwner(Owner owner);
        bool AddTransaction(Transaction transaction);
        bool AddTransfer(Transfer transfer, string transaction);
        bool UpdateBarge(Barge barge);
        bool UpdateOwner(Owner owner);
        bool UpdateTransfer(Transfer transfer);
        bool UpdateTransaction(double? transactionInput, string status, double Rate);
        bool DeleteTransaction(double? transactionInput);
    }
}
