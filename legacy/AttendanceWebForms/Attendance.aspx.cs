using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI.WebControls;

namespace AttendanceWebForms
{
    public partial class Attendance : System.Web.UI.Page
    {
        // 問題点①: 接続文字列がソースコードに直書き。
        // パスワードが平文でリポジトリに残り続ける。
        private string connStr = "Server=192.168.1.10;Database=KINMU;User Id=sa;Password=p@ssw0rd;";

        // =====================================================
        // 問題点②: Page_Load に処理が集中している。
        //
        // 初期表示・PostBack後の集計更新・権限チェックがすべてここに混在し、
        // 「今どのパスを通っているか」を !IsPostBack で分岐するしかない。
        // 処理が増えるほど !IsPostBack の入れ子が深くなっていく。
        // テストのしようがない。
        // =====================================================
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // 初回アクセス時のみ部署マスタを取得
                LoadDepartments();

                // 初回アクセス時のみデフォルト社員を設定
                LoadEmployees("dev");
                txtEmployeeId.Text = "EMP-001";
                lblEmployeeName.Text = "山田 太郎";
            }

            // 問題点③: !IsPostBack に関係なく毎リクエスト集計クエリが走る。
            // 出勤ボタンを押しても、部署を変えても、レポートを出力しても、
            // 必ずこのメソッドが呼ばれDB集計が実行される。
            LoadMonthlySummary(txtEmployeeId.Text);
        }

        private void LoadDepartments()
        {
            // 問題点④: SQL文字列結合。この例はパラメータなしだが、
            // 検索条件が入ると途端にインジェクションリスクになる。
            string sql = "SELECT DeptCode, DeptName FROM M_Department WHERE DeleteFlg = 0";
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // 問題点⑤: DataTable を ViewState に保持。
                // 部署マスタが増えるほど __VIEWSTATE が肥大化する。
                ViewState["DepartmentData"] = dt;

                ddlDepartment.DataSource    = dt;
                ddlDepartment.DataTextField  = "DeptName";
                ddlDepartment.DataValueField = "DeptCode";
                ddlDepartment.DataBind();
            }
        }

        private void LoadEmployees(string deptCode)
        {
            // 問題点⑥: 検索条件を文字列結合で組み立てている。
            // deptCode に「' OR '1'='1」を渡すと全社員が取得できてしまう。
            string sql = "SELECT EmpNo, EmpName FROM M_Employee WHERE DeptCode = '" + deptCode + "' AND DeleteFlg = 0";
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // 問題点⑦: 社員リストも ViewState に保持。
                ViewState["EmployeeData"] = dt;

                lstEmployees.DataSource    = dt;
                lstEmployees.DataTextField  = "EmpName";
                lstEmployees.DataValueField = "EmpNo";
                lstEmployees.DataBind();
            }
        }

        private void LoadMonthlySummary(string empNo)
        {
            if (string.IsNullOrEmpty(empNo)) return;

            // 問題点⑧: 集計SQLも文字列結合。
            string sql = "SELECT COUNT(DISTINCT AttDate) AS WorkDays, " +
                         "SUM(WorkMinutes) / 60.0 AS TotalHours, " +
                         "SUM(OvertimeMinutes) / 60.0 AS OvertimeHours " +
                         "FROM T_Attendance " +
                         "WHERE EmpNo = '" + empNo + "' " +
                         "AND FORMAT(AttDate, 'yyyyMM') = '" + DateTime.Now.ToString("yyyyMM") + "'";

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlDataReader dr = new SqlCommand(sql, conn).ExecuteReader();
                if (dr.Read())
                {
                    lblDays.Text     = dr["WorkDays"].ToString()     + " 日";
                    lblHours.Text    = dr["TotalHours"].ToString()   + " 時間";
                    lblOvertime.Text = dr["OvertimeHours"].ToString() + " 時間";
                }
            }
        }

        // =====================================================
        // 問題点⑨: AutoPostBack によるイベントハンドラ。
        // 部署を選択するたびにこのメソッドが呼ばれ、
        // その前に Page_Load も実行される。
        // つまり1回の部署選択で LoadMonthlySummary が2回走る。
        // =====================================================
        protected void ddlDepartment_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadEmployees(ddlDepartment.SelectedValue);
        }

        protected void lstEmployees_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtEmployeeId.Text  = lstEmployees.SelectedValue;
            lblEmployeeName.Text = lstEmployees.SelectedItem.Text;
        }

        // =====================================================
        // 問題点⑩: 打刻ロジックがボタンイベントに直書き。
        // Service層もRepository層もなく、テスト不能。
        // 4つのボタンでほぼ同じSQL処理が繰り返されている。
        // =====================================================
        protected void btnClockIn_Click(object sender, EventArgs e)
        {
            string sql = "INSERT INTO T_AttendanceLog (EmpNo, LogType, LogTime) VALUES ('" +
                         txtEmployeeId.Text + "', '出勤', '" +
                         DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')";
            ExecuteNonQuery(sql);
            lblStatus.Text = "出勤を記録しました。";
        }

        protected void btnClockOut_Click(object sender, EventArgs e)
        {
            string sql = "INSERT INTO T_AttendanceLog (EmpNo, LogType, LogTime) VALUES ('" +
                         txtEmployeeId.Text + "', '退勤', '" +
                         DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')";
            ExecuteNonQuery(sql);
            lblStatus.Text = "退勤を記録しました。";
        }

        protected void btnBreakStart_Click(object sender, EventArgs e)
        {
            string sql = "INSERT INTO T_AttendanceLog (EmpNo, LogType, LogTime) VALUES ('" +
                         txtEmployeeId.Text + "', '休憩開始', '" +
                         DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')";
            ExecuteNonQuery(sql);
            lblStatus.Text = "休憩開始を記録しました。";
        }

        protected void btnBreakEnd_Click(object sender, EventArgs e)
        {
            string sql = "INSERT INTO T_AttendanceLog (EmpNo, LogType, LogTime) VALUES ('" +
                         txtEmployeeId.Text + "', '休憩終了', '" +
                         DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')";
            ExecuteNonQuery(sql);
            lblStatus.Text = "休憩終了を記録しました。";
        }

        private void ExecuteNonQuery(string sql)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                // 問題点⑪: try-catch なし。例外がそのままユーザーに見える。
                new SqlCommand(sql, conn).ExecuteNonQuery();
            }
        }

        // =====================================================
        // 問題点⑫: Response.Write によるCSV直接出力。
        //
        // - 文字コードが Shift-JIS 決め打ちのため環境によって文字化けする。
        // - Response.Write を開始した後にエラーが起きても
        //   画面にエラーメッセージを表示できない。
        // - ダウンロード中にページが白くなり、完了しても何も起きない。
        // =====================================================
        protected void btnReport_Click(object sender, EventArgs e)
        {
            string yearMonth = txtReportMonth.Text;

            Response.Clear();
            Response.ContentType = "text/csv";
            Response.Charset = "shift_jis";
            Response.AddHeader("Content-Disposition", "attachment; filename=report_" + yearMonth + ".csv");

            Response.Write("社員番号,氏名,出勤日数,合計時間,残業時間\r\n");

            string sql = "SELECT e.EmpNo, e.EmpName, " +
                         "COUNT(DISTINCT a.AttDate) AS WorkDays, " +
                         "SUM(a.WorkMinutes) / 60.0 AS TotalHours, " +
                         "SUM(a.OvertimeMinutes) / 60.0 AS OvertimeHours " +
                         "FROM M_Employee e " +
                         "LEFT JOIN T_Attendance a ON e.EmpNo = a.EmpNo " +
                         "AND FORMAT(a.AttDate, 'yyyyMM') = '" + yearMonth + "' " +
                         "GROUP BY e.EmpNo, e.EmpName " +
                         "ORDER BY e.EmpNo";

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlDataReader dr = new SqlCommand(sql, conn).ExecuteReader();
                while (dr.Read())
                {
                    Response.Write(dr["EmpNo"] + "," + dr["EmpName"] + "," +
                                   dr["WorkDays"] + "," + dr["TotalHours"] + "," +
                                   dr["OvertimeHours"] + "\r\n");
                }
            }

            Response.End();
        }
    }
}
