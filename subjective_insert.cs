        public List<subjectiveConfig> GetSubjectiveItemWeight(string station_id, string title, string item)
        {
            string sql = "select a.weighting,replace(replace(a.remark,'''','&#39;'),'\"','&quot;') as remark,a.upperbound,a.lowerbound from Rbl_DL_item a where station_id in (" + station_id + ")" + "and title = :Title and item= :Item";
            return _dbConnection.Query<subjectiveConfig>(sql, new { Station_id = station_id, Title = title, Item = item }).ToList();
        }

        public bool SubjectiveSave(string emp_id, string year, string month, string item, string detailItem, int record, decimal score, string comments, string title, int totalCount, string userid)
        {
            // 對應 JSP Line 120 (Update) 和 Line 124 (Insert) 
            string Querysql = string.Empty;
            string sql = string.Empty;

            Querysql = @"select *
                      from Rbl_DL_Performance_subject
                     where emp_id = :Emp_id
                       and year = :Year
                       and month = :Month
                       and item = :Item
                       and detailitem = :DetailItem";

            var QueryResult = _dbConnection.Query<SubjectiveVM>(Querysql, new { Emp_id = emp_id, Year = year, Month = month, Item = item, DetailItem = detailItem }).ToList();

            if (QueryResult.Count > 0)
            {
                sql = @"
                    UPDATE Rbl_DL_Performance_subject 
                    SET record = @Record, score = @Score, comments = @Comments,
                        rankinga = FUN_GET_SUBJECT_RANKINGA(@Title, @Record, @TotalCount),
                        updateuserid = @ModifierId, updatetime = TO_CHAR(SYSDATE,'yyyymmdd hh24miss') || '000'
                    WHERE emp_id = :Emp_id
                           and year = :Year
                           and month = :Month
                           and item = :Item
                           and detailitem = :DetailItem";
            }
            else
            {
                sql = @"
                    INSERT INTO Rbl_DL_Performance_subject 
                    (emp_id, year, month, item, detailitem, record, score, comments, rankinga, createuserid, createtime)
                    VALUES 
                    (:EmpId, :Year, :Month, :Item, :DetailItem, :Record, :Score, :Comments, 
                     FUN_GET_SUBJECT_RANKINGA(:Title, :Record, :TotalCount), 
                     :ModifierId, TO_CHAR(SYSDATE,'yyyymmdd hh24miss')||'000')";
            }
            int resultcount = _dbConnection.Execute(sql, new { Emp_id = emp_id, Year = year, Month = month, Item = item, DetailItem = detailItem, Record = record, Score = score, Comments = comments, Title = title, Totalcount = totalCount, ModifierId = userid });
            
            if(resultcount > 0) //表示更新成功
            {
                return true;
            }
            else
            {
                return false;
            }
        }
