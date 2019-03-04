from pages.base import CZBTests

class Wait_Page(CZBTests):

    def __init__(self,altdriver):
        self.altdriver=altdriver
        wait_page=self.altdriver.wait_for_element('LoginPopup(Clone)/Waiting_Group')
        self.altdriver.wait_for_element_to_not_be_present('LoginPopup(Clone)/'+wait_page.name)