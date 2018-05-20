# How to contribute

Please read these guidelines before contributing to Json.NET:

 - [Question or Problem?](#question)
 - [Issues and Bugs](#issue)
 - [Feature Requests](#feature)
 - [Submitting a Pull Request](#pullrequest)
 - [Contributor License Agreement](#cla)


## <a name="question"></a> Got a Question or Problem?

If you have questions about how to use Json.NET, please read the
[Json.NET documentation][documentation] or ask on [Stack Overflow][stackoverflow]. There are
thousands of Json.NET questions on Stack Overflow with the [json.net][stackoverflow] tag.

GitHub issues are only for [reporting bugs](#issue) and [feature requests](#feature), not
questions or help.


## <a name="issue"></a> Found an Issue?

If you find a bug in the source code or a mistake in the documentation, you can help by
submitting an issue to the [GitHub Repository][github]. Even better you can submit a Pull Request
with a fix.

When submitting an issue please include the following information:

- A description of the issue
- The JSON, classes, and Json.NET code related to the issue
- The exception message and stacktrace if an error was thrown
- If possible, please include code that reproduces the issue. [DropBox][dropbox] or GitHub's
[Gist][gist] can be used to share large code samples, or you could
[submit a pull request](#pullrequest) with the issue reproduced in a new test.

The more information you include about the issue, the more likely it is to be fixed!


## <a name="feature"></a> Want a Feature?

You can request a new feature by submitting an issue to the [GitHub Repository][github]. Before
requesting a feature consider the following:

- Json.NET has many extensibility points, it is possible you can implement your feature today without
modifying Json.NET
- Stability is important. Json.NET is used by thousands of other libraries and features that require
large breaking changes are unlikely to be accepted


## <a name="pullrequest"></a> Submitting a Pull Request

When submitting a pull request to the [GitHub Repository][github] make sure to do the following:

- Check that new and updated code follows Json.NET's existing code formatting and naming standard
- Run Json.NET's unit tests to ensure no existing functionality has been affected
- Write new unit tests to test your changes. All features and fixed bugs must have tests to verify
they work

Read [GitHub Help][pullrequesthelp] for more details about creating pull requests.


## <a name="cla"></a> Contributor License Agreement

By contributing your code to Json.NET you grant James Newton-King a non-exclusive, irrevocable, worldwide,
royalty-free, sublicenseable, transferable license under all of Your relevant intellectual property rights
(including copyright, patent, and any other rights), to use, copy, prepare derivative works of, distribute and
publicly perform and display the Contributions on any licensing terms, including without limitation:
(a) open source licenses like the MIT license; and (b) binary, proprietary, or commercial licenses. Except for the
licenses granted herein, You reserve all right, title, and interest in and to the Contribution.

You confirm that you are able to grant us these rights. You represent that You are legally entitled to grant the
above license. If Your employer has rights to intellectual property that You create, You represent that You have
received permission to make the Contributions on behalf of that employer, or that Your employer has waived such
rights for the Contributions.

You represent that the Contributions are Your original works of authorship, and to Your knowledge, no other person
claims, or has the right to claim, any right in any invention or patent related to the Contributions. You also
represent that You are not legally obligated, whether by entering into an agreement or otherwise, in any way that
conflicts with the terms of this license.

James Newton-King acknowledges that, except as explicitly described in this Agreement, any Contribution which
you provide is on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED,
INCLUDING, WITHOUT LIMITATION, ANY WARRANTIES OR CONDITIONS OF TITLE, NON-INFRINGEMENT, MERCHANTABILITY, OR FITNESS
FOR A PARTICULAR PURPOSE.


[github]: https://github.com/JamesNK/Newtonsoft.Json
[documentation]: https://www.newtonsoft.com/json/help
[stackoverflow]: https://stackoverflow.com/questions/tagged/json.net
[dropbox]: https://www.dropbox.com
[gist]: https://gist.github.com
[pullrequesthelp]: https://help.github.com/articles/using-pull-requests
