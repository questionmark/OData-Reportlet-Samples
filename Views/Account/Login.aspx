<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="ViewPage<AccountModel>" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadPlaceHolder" runat="server">
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="BodyPlaceHolder" runat="server">

  <div class="span5">

    <% using (Html.BeginBootstrapHorizontalForm(new { ViewBag.ReturnUrl })) { %>

      <%: Html.AntiForgeryToken() %>
      
      &nbsp;

      <div class="control-group">
        <%: Html.BootstrapControlLabelFor(m => m.TenantId) %>
        <div class="controls">
          <%: Html.TextBoxFor(m => m.TenantId) %>
          <div class="text-error">
            <%: Html.ValidationMessageFor(m => m.TenantId) %>
          </div>
        </div>
      </div>

      <div class="control-group">
        <%: Html.BootstrapControlLabelFor(m => m.Username) %>
        <div class="controls">
          <%: Html.TextBoxFor(m => m.Username) %>
          <div class="text-error">
            <%: Html.ValidationMessageFor(m => m.Username) %>
          </div>
        </div>
      </div>

      <div class="control-group">
        <%: Html.BootstrapControlLabelFor(m => m.Password) %>
        <div class="controls">
          <%: Html.PasswordFor(m => m.Password) %>
          <div class="text-error">
            <%: Html.ValidationMessageFor(m => m.Password) %>
          </div>
        </div>
      </div>
    
      <div class="control-group">
        <div class="controls">
          <label class="checkbox">
            <%: Html.BootstrapCheckboxFor(m => m.RememberMe) %> <%: Html.LabelFor(m => m.RememberMe) %>
          </label>
        </div>
      </div>

      <div class="control-group">
        <div class="controls">
          <input type="submit" value="Login" class="btn" />
        </div>
      </div>

    <% } %>

  </div>

</asp:Content>

<asp:Content ID="ScriptContent" ContentPlaceHolderID="ScriptPlaceHolder" runat="server">
  <script type="text/javascript">
    $(function () {
      $('#TenantId').focus();
    });
  </script>
</asp:Content>