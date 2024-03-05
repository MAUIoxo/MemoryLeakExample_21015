﻿using MvvmHelpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MemoryLeakExampleDatabase
{
    public class Store : ObservableObject
    {
        [Key]
        [Column(Order = 1)]
        public int Id { get; set; }


        #region Name

        private string _name;

        [Required]
        [Column(Order = 2, TypeName = "TEXT COLLATE NOCASE")]              // Ignore case sensitivity for the Unique Constraint
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        #endregion

        #region Street

        private string _street;

        [Column(Order = 3)]
        public string Street
        {
            get => _street;
            set => SetProperty(ref _street, value);
        }

        #endregion

        #region Manager

        private double _manager;

        [Column(Order = 4)]
        public double Manager
        {
            get => _manager;
            set => SetProperty(ref _manager, value);
        }

        #endregion

        #region SalesPerson

        private double _salesPerson;

        [Column(Order = 5)]
        public double SalesPerson
        {
            get => _salesPerson;
            set => SetProperty(ref _salesPerson, value);
        }

        #endregion

        #region Clerk

        private double _clerk;

        [Column(Order = 6)]
        public double Clerk
        {
            get => _clerk;
            set => SetProperty(ref _clerk, value);
        }

        #endregion


        #region StoreSelection

        private List<StoreSelection> _storeSelections;
        public virtual List<StoreSelection> StoreSelections
        {
            get => this._storeSelections ?? (this._storeSelections = new List<StoreSelection>());
            set => SetProperty(ref _storeSelections, value);
        }

        #endregion
    }
}
