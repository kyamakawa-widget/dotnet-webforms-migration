<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Attendance.aspx.cs"
    Inherits="AttendanceWebForms.Attendance" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN"
    "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>勤怠打刻画面 - 社内勤怠管理システム</title>
    <link rel="stylesheet" type="text/css" href="style.css" />
</head>
<body>
<form id="form1" runat="server">

    <%-- =====================================================
         問題点①: __VIEWSTATE がこのフォーム全体に対して生成される。
         asp:GridView や DataSet を ViewState に保持すると
         hiddenフィールドのサイズが数十KBに膨張する。
         ===================================================== --%>

    <div class="page-header">
        社内勤怠管理システム
        <span>勤怠打刻画面</span>
    </div>

    <div class="panel">
        <div class="panel-title">■ 社員情報</div>
        <div class="panel-body">
            <table>
                <tr>
                    <td>社員番号：</td>
                    <td>
                        <%-- サーバーコントロール: runat="server" で ViewState に紐づく --%>
                        <asp:TextBox ID="txtEmployeeId" runat="server" />
                    </td>
                </tr>
                <tr>
                    <td>部署：</td>
                    <td>
                        <%-- 問題点②: AutoPostBack=true
                             ドロップダウンを選択するたびにフォーム全体がサーバーへ
                             POST される。ページ全体がリロードされ、スクロール位置も
                             入力途中のデータもリセットされる。 --%>
                        <asp:DropDownList ID="ddlDepartment" runat="server"
                            AutoPostBack="true"
                            OnSelectedIndexChanged="ddlDepartment_SelectedIndexChanged" />
                    </td>
                </tr>
                <tr>
                    <td>社員名：</td>
                    <td>
                        <asp:Label ID="lblEmployeeName" runat="server" />
                    </td>
                </tr>
                <tr>
                    <td>社員一覧：</td>
                    <td>
                        <%-- 問題点③: このリストのデータも ViewState に保持される。
                             社員数が増えるほどリクエストサイズが膨張する。 --%>
                        <asp:ListBox ID="lstEmployees" runat="server" Rows="3"
                            OnSelectedIndexChanged="lstEmployees_SelectedIndexChanged"
                            AutoPostBack="true" />
                    </td>
                </tr>
            </table>
        </div>
    </div>

    <div class="panel">
        <div class="panel-title">■ 打刻操作</div>
        <div class="panel-body">
            <%-- 問題点④: ボタンクリックのたびに PostBack が発生する。
                 btnClockIn_Click の中で SQL を直接組み立てて実行している。
                 処理完了後も Page_Load が再度走り、全集計クエリが実行される。 --%>
            <asp:Button ID="btnClockIn"    runat="server" Text="出勤"
                OnClick="btnClockIn_Click" />
            <asp:Button ID="btnClockOut"   runat="server" Text="退勤"
                OnClick="btnClockOut_Click" />
            <asp:Button ID="btnBreakStart" runat="server" Text="休憩開始"
                OnClick="btnBreakStart_Click" />
            <asp:Button ID="btnBreakEnd"   runat="server" Text="休憩終了"
                OnClick="btnBreakEnd_Click" />

            <asp:Label ID="lblStatus" runat="server" />
        </div>
    </div>

    <div class="panel">
        <div class="panel-title">■ 今月の集計</div>
        <div class="panel-body">
            <%-- 問題点⑤: Page_Load のたびにここの値を更新するため、
                 どのボタンを押しても・部署を変えても
                 必ずDB集計クエリが実行される。 --%>
            <table>
                <tr><td>出勤日数：</td><td><asp:Label ID="lblDays"     runat="server" /></td></tr>
                <tr><td>合計時間：</td><td><asp:Label ID="lblHours"    runat="server" /></td></tr>
                <tr><td>残業時間：</td><td><asp:Label ID="lblOvertime" runat="server" /></td></tr>
            </table>
        </div>
    </div>

    <div class="panel">
        <div class="panel-title">■ 月次レポート</div>
        <div class="panel-body">
            <asp:TextBox ID="txtReportMonth" runat="server" Text="202605" />
            <%-- 問題点⑥: このボタンのクリックハンドラで Response.Write を使い
                 CSV を直接ストリーム出力する。文字コードは Shift-JIS 決め打ち。
                 エラー時も Response.Write 済みのため画面にエラーを表示できない。 --%>
            <asp:Button ID="btnReport" runat="server" Text="月次レポート出力"
                OnClick="btnReport_Click" />
        </div>
    </div>

</form>
</body>
</html>
