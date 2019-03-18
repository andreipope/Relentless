from .base import CZBTests
import time
import re


class Open_Packs_Page(CZBTests):
    
    def __init__(self,altdriver,driver):
        self.altdriver=altdriver
        self.driver=driver
        self.back_button=self.altdriver.wait_for_element('PackOpenerPage(Clone)/Header/BackButton')
        self.buy_packs_button=self.altdriver.wait_for_element('PackOpenerPage(Clone)/Header/Button_BuyPacks')
        self.open_pack_button=self.altdriver.wait_for_element('PackOpenerPage(Clone)/Pack_Panel/RightPanel/ButtonOpenPacks')

    def get_card_from_pack(self):
        cards=[]
        self.altdriver.wait_for_element('OpenPackGooPool(Clone)')
        elements_with_card_in_name=self.altdriver.find_elements_where_name_contains('Card')
        for element in elements_with_card_in_name:
            if re.match(r"Card \([0-9]\)", element.name) or element.name=="Card":
                cards.append(element)
        cards.sort(key=lambda element:float(element.x))
        return cards
    
    def tap_every_card(self,cards):
        for card in cards:
            print(card.name+' '+str(card.x))
            card.mobile_tap()
            time.sleep(1)

    def collect_cards(self):
        self.altdriver.wait_for_element('Panel_Collect/ButtonCollect').mobile_tap()


    def open_pack(self):
        self.open_pack_button.mobile_tap()
        time.sleep(12)

        cards=self.get_card_from_pack()
        for card in cards:
            print(card.name)
        self.tap_every_card(cards)

        self.collect_cards()
        time.sleep(2)
    def press_back_button(self):
        print(self.back_button.x,self.back_button.y,self.back_button.mobileY)
        self.button_pressed(self.back_button)
