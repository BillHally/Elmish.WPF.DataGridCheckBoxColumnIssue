# Short script to demonstrate an Elmish.WPF issue

## Running the script

The script can be run with either the current release of Elmish.WPF or with
[a fix](https://github.com/BillHally/Elmish.WPF/commit/db90171bb71328ca80bb02afcd54b53298c8ecdc).

* To show current behavior:

    ```powershell
    fsi Issue.fsx
    ```

* To show behavior with the fix:

    ```powershell
    fsi --define:USE_FIX Issue.fsx
    ```

## Things to look for

1. Entering text into the `TextBox` behaves as expected (should be true for
   both versions)
2. When `USE_FIX` is *not* defined, toggling checkboxes causes them to be
   displayed with the validation template incorrectly indicating an error has
   occurred
3. When `USE_FIX` *is* defined, toggling checkboxes causes them to be
   displayed correctly i.e. without indicating a validation error