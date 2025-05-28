        public ActionResult GetSortedTable(string empId, string stationId, string certItemId, string month, string sortType)
        {
            string GetEmpIdShift = _TestingService.GetShiftByEmpId(userId);
            var list = _TestingService.GetEmployeeCertifications(SESGroup, GetEmpIdShift, sortType);
            return PartialView("_EmployeeTable", list);
        }
