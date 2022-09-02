/* Sets up user groups and policies (permissions) for each group */

/*
// full access for developers
resource "aws_iam_group" "developers" {
  name = "developers"
  path = "/users/"
}

resource "aws_iam_policy_attachment" "developer_access" {
  name       = "developer_access"
  groups     = [aws_iam_group.developers.name]
  policy_arn = "arn:aws:iam::aws:policy/AdministratorAccess"
}


// IAM read only access 
resource "aws_iam_group" "IAM_read_only_access" {
  name = "user_read_only"
  path = "/users/"
}

resource "aws_iam_policy_attachment" "read_only_access" {
  name       = "read_only_access"
  groups     = [aws_iam_group.IAM_read_only_access.name]
  policy_arn = "arn:aws:iam::aws:policy/IAMReadOnlyAccess"
}


// user management access
resource "aws_iam_group" "user_managers" {
  name = "user_managers"
  path = "/users/"
}

resource "aws_iam_group_policy_attachment" "user_manager_policy" {
  group      = aws_iam_group.developers.name
  policy_arn = data.aws_iam_policy_document.manage_group_access.json
}

data "aws_iam_policy_document" "manage_group_access" {
  statement {
    sid    = "ViewGroups"
    effect = "Allow"
    actions = [
      "iam:ListGroups",
      "iam:ListUsers",
      "iam:GetUser",
      "iam:ListGroupsForUser"
    ]
    resources = ["*"]
  }

  statement {
    sid    = "ViewEditThisGroup"
    effect = "Allow"
    actions = [
      "iam:AddUserToGroup",
      "iam:RemoveUserFromGroup",
      "iam:GetGroup"
    ]
    resources = ["*"]
  }

  statement {
    sid    = "ReadAccessToConsole"
    effect = "Allow"
    actions = [
      "iam:Get*",
      "iam:List*",
      "iam:Generate*"
    ]
    resources = ["*"]
  }

  statement {
    sid    = "GenerateIAMCredentialReports"
    effect = "Allow"
    actions = [
      "iam:GenerateCredentialReport",
      "iam:GetCredentialReport"
    ]
    resources = ["*"]
  }
}
*/
