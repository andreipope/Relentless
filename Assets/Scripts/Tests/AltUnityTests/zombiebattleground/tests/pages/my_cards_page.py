from .base import CZBTests


class My_Cards_Page(CZBTests):
    
    def __init__(self,altdriver,driver):
        self.altdriver=altdriver
        self.driver=driver
        self.my_cards_page=self.altdriver.wait_for_element('MyCardsPage(Clone)')

    
